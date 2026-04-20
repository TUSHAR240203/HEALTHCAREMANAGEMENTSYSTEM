using System.Collections.Generic;

namespace Frontend.Models.Reception
{
    public class DepartmentQueueResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public List<QueueItemDto> Queue { get; set; } = new();
    }
}
