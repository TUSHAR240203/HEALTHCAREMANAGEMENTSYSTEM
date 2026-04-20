using System.Collections.Generic;

namespace Frontend.Models.Reception
{
    public class ReceptionPatientSearchResponseDto
    {
        public List<ReceptionPatientSummaryDto> Patients { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
