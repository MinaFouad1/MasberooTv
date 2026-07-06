using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class LookupMapper
{
    public static SecurityClearanceStatusDto ToDto(this SecurityClearanceStatus s) => new(s.Id, s.Name);
    public static StatusDto                  ToDto(this Status s)                  => new(s.Id, s.Name);
    public static InterviewTypeDto           ToDto(this InterviewType t)            => new(t.Id, t.Name);
    public static PhaseDto                   ToDto(this Phase p)                    => new(p.Id, p.Name);
    public static ProcessStatusDto           ToDto(this ProcessStatus ps)           => new(ps.Id, ps.Name, ps.Percentage);
}
