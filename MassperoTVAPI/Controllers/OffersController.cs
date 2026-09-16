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
    private const string ProfileConfigurationKey = "ProfileImage";

    // ── Candidate status names that drive automatic status transitions ─────────
    private const string StatusInOffer   = "Offered";
    private const string StatusHired     = "Hired";
    private const string StatusRejected  = "Rejected";
    private const string StatusInProcess = "In Process";

    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    public OffersController(IUnitOfWork uow, IEmailService emailService)
    {
        _uow = uow;
        _emailService = emailService;
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
        var profileBaseUrl = (await _uow.Configurations
            .GetByKeyAsync(ProfileConfigurationKey))?.Value;

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
            items.Select(o => o.ToDto(profileBaseUrl)),
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
    
    /// <summary>
    /// Emails an offer document as an attachment, creates a pending offer record,
    /// and moves the candidate to "Offered" only after the email is sent.
    /// </summary>
    [HttpPost("send")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> SendOffer([FromForm] SendOfferDto dto)
    {
        if (!ModelState.IsValid || dto.OfferFile.Length == 0)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                "CandidateId and a non-empty OfferFile are required."));

        var candidate = await _uow.Candidates.GetByIdAsync(dto.CandidateId);
        if (candidate is null)
            return NotFound(ApiResponse<OfferDto>.NotFoundResponse(
                $"Candidate {dto.CandidateId} not found."));

        if (string.IsNullOrWhiteSpace(candidate.Email))
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                "The candidate does not have an email address."));

        var offeredStatus = await GetStatusByNameAsync(StatusInOffer);
        if (offeredStatus is null)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                $"Status '{StatusInOffer}' not found in the database."));

        byte[] document;
        await using (var stream = new MemoryStream())
        {
            await dto.OfferFile.CopyToAsync(stream);
            document = stream.ToArray();
        }

        var subject = string.IsNullOrWhiteSpace(dto.Subject)
            ? "Your employment offer"
            : dto.Subject.Trim();
        var message = string.IsNullOrWhiteSpace(dto.Message)
            ? "Please find your offer document attached."
            : dto.Message.Trim();
        var encodedName = System.Net.WebUtility.HtmlEncode(candidate.Name);
        var encodedMessage = System.Net.WebUtility.HtmlEncode(message)
            .Replace(Environment.NewLine, "<br/>");

        try
        {
            await _emailService.SendWithAttachmentAsync(
                candidate.Email,
                candidate.Name,
                subject,
                $"<p>Dear {encodedName},</p><p>{encodedMessage}</p>",
                document,
                dto.OfferFile.FileName,
                dto.OfferFile.ContentType);
        }
        catch
        {
            return StatusCode(500, ApiResponse<OfferDto>.ErrorResponse(
                "The offer email could not be sent. The candidate status was not changed."));
        }

        var offer = new Offer
        {
            CandidateId = candidate.Id,
            OfferStatusId = OfferStatus.Pending,
            ProposedSalary = dto.ProposedSalary ?? 0m,
            ExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddDays(30),
            StartDate = dto.StartDate,
            Benefits = dto.Benefits,
            OfferDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = User.Identity?.Name
        };

        await _uow.Offers.AddAsync(offer);
        candidate.StatusId = offeredStatus.Id;
        _uow.Candidates.Update(candidate);
        await _uow.SaveChangesAsync();

        var created = await _uow.Offers.GetByIdWithDetailsAsync(offer.Id);
        return StatusCode(201, ApiResponse<OfferDto>.CreatedResponse(
            created!.ToDto(), "Offer email sent and candidate moved to 'Offered'."));
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

    /// <summary>Updates an offer status. A decline requires ReasonOfRejected.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> UpdateStatus(
        int id,
        [FromBody] UpdateOfferStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse("Validation failed."));

        var newStatus = (OfferStatus)dto.OfferStatusId;
      

        return await TransitionOffer(id, newStatus, dto.ReasonOfRejected);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PATCH /api/offers/{id}/decline
    //  Decline offer  →  candidate status → "Rejected"
    // ─────────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────────
    //  PATCH /api/offers/{id}/expire
    //  Expire offer  →  candidate status → "In Process"
    // ─────────────────────────────────────────────────────────────────────────

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

    /// <summary>Updates an offer status and keeps the candidate status in sync.</summary>
    private async Task<ActionResult<ApiResponse<OfferDto>>> TransitionOffer(
        int offerId,
        OfferStatus newOfferStatus,
        string? reasonOfRejected)
    {
        var offer = await _uow.Offers.GetByIdWithDetailsAsync(offerId);
        if (offer is null)
            return NotFound(ApiResponse<OfferDto>.NotFoundResponse());

        if (offer.OfferStatusId == newOfferStatus)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse($"Offer is already {newOfferStatus}."));

        var candidateStatusName = newOfferStatus switch
        {
            OfferStatus.Accepted => StatusHired,
            OfferStatus.Declined => StatusRejected,
            OfferStatus.Expired  => StatusInProcess,
            _                    => StatusInOffer
        };

        var targetStatus = await GetStatusByNameAsync(candidateStatusName);
        if (targetStatus is null)
            return BadRequest(ApiResponse<OfferDto>.ErrorResponse(
                $"Status '{candidateStatusName}' not found in the database."));

        offer.OfferStatusId = newOfferStatus;
        offer.ReasonOfRejected = newOfferStatus == OfferStatus.Declined
            ? reasonOfRejected?.Trim()
            : null;
        _uow.Offers.Update(offer);

       

        await _uow.SaveChangesAsync();

        var updated = await _uow.Offers.GetByIdWithDetailsAsync(offer.Id);
        return Ok(ApiResponse<OfferDto>.SuccessResponse(
            updated!.ToDto(),
            "Offer status updated successfully."));
    }

    private async Task<Status?> GetStatusByNameAsync(string name)
        => (await _uow.Statuses.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
}
