using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/offers")]
[Authorize(Roles = "HR,Admin")]
public class OffersController : ControllerBase
{
    // ── Candidate status names that drive automatic status transitions ─────────
    private const string StatusInOffer   = "Offered";
    private const string StatusHired     = "Hired";
    private const string StatusRejected  = "Rejected";
    private const string StatusInProcess = "In Process";

    private readonly IUnitOfWork _uow;

    public OffersController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GET /api/offers/summary
    //  Dashboard stats cards: Total / Pending / Accepted / Declined / Expired
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns offer status counts for the dashboard summary cards.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<OfferStatusSummaryDto>>> GetSummary()
    {
        var summary = await _uow.Offers.GetStatusSummaryAsync();
        return Ok(ApiResponse<OfferStatusSummaryDto>.SuccessResponse(summary));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GET /api/offers
    //  Paged + filtered list (matches the table in the UI)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paged, filtered list of offers.
    /// Filters: candidateName, offerStatusId, categoryId, jobId, dateFrom, dateTo.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OfferDto>>>> GetAll(
        [FromQuery] GetOffersQueryDto query)
    {
        var safePage     = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await _uow.Offers.GetPagedAsync(
            query.CandidateName,
            query.OfferStatusId,
            query.CategoryId,
            query.JobId,
            query.DateFrom,
            query.DateTo,
            safePage,
            safePageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        var result = new PagedResult<OfferDto>(
            items.Select(o => o.ToDto()),
            totalCount,
            safePage,
            safePageSize,
            totalPages);

        return Ok(ApiResponse<PagedResult<OfferDto>>.SuccessResponse(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GET /api/offers/{id}
    //  Full offer preview (preview panel in the UI)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns the full offer preview including candidate summary and rank.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OfferPreviewDto>>> GetById(int id)
    {
        var offer = await _uow.Offers.GetByIdWithDetailsAsync(id);
        if (offer is null)
            return NotFound(ApiResponse<OfferPreviewDto>.NotFoundResponse());

        var rank = await _uow.Offers.GetCandidateRankAsync(
            offer.CandidateId,
            offer.Candidate.JobId);

        return Ok(ApiResponse<OfferPreviewDto>.SuccessResponse(offer.ToPreviewDto(rank)));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  POST /api/offers
    //  Create offer  →  candidate status → "In Offer"
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new offer for a candidate.
    /// Automatically moves the candidate's status to "In Offer".
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OfferDto>>> Create([FromBody] CreateOfferDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse("Validation failed."));

        // Validate candidate exists
        var candidate = await _uow.Candidates.GetByIdWithDetailsAsync(dto.CandidateId);
        if (candidate is null)
            return NotFound(ApiResponse<OfferDto>.NotFoundResponse($"Candidate {dto.CandidateId} not found."));

        // Resolve "In Offer" status
        var inOfferStatus = await GetStatusByNameAsync(StatusInOffer);
        if (inOfferStatus is null)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse($"Status '{StatusInOffer}' not found in the database."));

        // Validate expiry date
        if (dto.ExpiryDate <= DateTime.UtcNow)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse("ExpiryDate must be in the future."));

        // Create the offer
        var offer = new Offer
        {
            CandidateId    = dto.CandidateId,
            OfferStatusId  = OfferStatus.Pending,
            ProposedSalary = dto.ProposedSalary,
            StartDate      = dto.StartDate,
            ExpiryDate     = dto.ExpiryDate,
            Benefits       = dto.Benefits,
            OfferDate      = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
            CreatedBy      = User.Identity?.Name
        };

        await _uow.Offers.AddAsync(offer);

        // Move candidate status → "In Offer"
        candidate.StatusId = inOfferStatus.Id;
        _uow.Candidates.Update(candidate);

        await _uow.SaveChangesAsync();

        var created = await _uow.Offers.GetByIdWithDetailsAsync(offer.Id);
        return StatusCode(201, ApiResponse<OfferDto>.CreatedResponse(created!.ToDto()));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PUT /api/offers/{id}
    //  Update offer details (salary, dates, benefits)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Updates an existing offer's details (salary, start date, expiry date, benefits).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> Update(int id, [FromBody] UpdateOfferDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse("Validation failed."));

        var offer = await _uow.Offers.GetByIdAsync(id);
        if (offer is null)
            return NotFound(ApiResponse<OfferDto>.NotFoundResponse());

        // Only allow edits while the offer is still Pending
        if (offer.OfferStatusId != OfferStatus.Pending)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                $"Cannot edit an offer that is already {offer.OfferStatusId}."));

        offer.ProposedSalary = dto.ProposedSalary;
        offer.StartDate      = dto.StartDate;
        offer.ExpiryDate     = dto.ExpiryDate;
        offer.Benefits       = dto.Benefits;

        _uow.Offers.Update(offer);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Offers.GetByIdWithDetailsAsync(offer.Id);
        return Ok(ApiResponse<OfferDto>.SuccessResponse(updated!.ToDto()));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PATCH /api/offers/{id}/accept
    //  Accept offer  →  candidate status → "Hired"
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Marks the offer as Accepted.
    /// Automatically moves the candidate's status to "Hired".
    /// </summary>
    [HttpPatch("{id:int}/accept")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> Accept(int id)
        => await TransitionOffer(id, OfferStatus.Accepted, StatusHired, "Offer accepted. Candidate moved to 'Hired'.");

    // ─────────────────────────────────────────────────────────────────────────
    //  PATCH /api/offers/{id}/decline
    //  Decline offer  →  candidate status → "Rejected"
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Marks the offer as Declined.
    /// Automatically moves the candidate's status to "Rejected".
    /// </summary>
    [HttpPatch("{id:int}/decline")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> Decline(int id)
        => await TransitionOffer(id, OfferStatus.Declined, StatusRejected, "Offer declined. Candidate moved to 'Rejected'.");

    // ─────────────────────────────────────────────────────────────────────────
    //  PATCH /api/offers/{id}/expire
    //  Expire offer  →  candidate status → "In Process"
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Marks the offer as Expired.
    /// Automatically moves the candidate's status back to "In Process".
    /// </summary>
    [HttpPatch("{id:int}/expire")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> Expire(int id)
        => await TransitionOffer(id, OfferStatus.Expired, StatusInProcess, "Offer expired. Candidate moved to 'In Process'.");

    // ─────────────────────────────────────────────────────────────────────────
    //  DELETE /api/offers/{id}   (Admin only)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Deletes an offer. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await _uow.Offers.GetByIdAsync(id);
        if (offer is null)
            return NotFound(ApiResponse<object>.NotFoundResponse());

        try
        {
            _uow.Offers.Delete(offer);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Cannot delete this offer: {ex.Message}"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Offer deleted successfully."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generic offer status transition: updates OfferStatusId and syncs candidate status.
    /// </summary>
    private async Task<ActionResult<ApiResponse<OfferDto>>> TransitionOffer(
        int         offerId,
        OfferStatus newOfferStatus,
        string      candidateStatusName,
        string      successMessage)
    {
        var offer = await _uow.Offers.GetByIdWithDetailsAsync(offerId);
        if (offer is null)
            return NotFound(ApiResponse<OfferDto>.NotFoundResponse());

        if (offer.OfferStatusId == newOfferStatus)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse($"Offer is already {newOfferStatus}."));

        // Resolve target candidate status
        var targetStatus = await GetStatusByNameAsync(candidateStatusName);
        if (targetStatus is null)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                $"Status '{candidateStatusName}' not found in the database."));

        // Update offer status
        offer.OfferStatusId = newOfferStatus;
        _uow.Offers.Update(offer);

        // Update candidate status
        var candidate = await _uow.Candidates.GetByIdAsync(offer.CandidateId);
        if (candidate is not null)
        {
            candidate.StatusId = targetStatus.Id;

            // If accepted, set hiring date
            if (newOfferStatus == OfferStatus.Accepted)
                candidate.HiringDate = DateTime.UtcNow;

            _uow.Candidates.Update(candidate);
        }

        await _uow.SaveChangesAsync();

        var updated = await _uow.Offers.GetByIdWithDetailsAsync(offer.Id);
        return Ok(ApiResponse<OfferDto>.SuccessResponse(updated!.ToDto(), successMessage));
    }

    private async Task<Status?> GetStatusByNameAsync(string name)
        => (await _uow.Statuses.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
}
