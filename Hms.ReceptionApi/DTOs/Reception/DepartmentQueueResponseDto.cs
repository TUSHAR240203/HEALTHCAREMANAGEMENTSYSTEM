namespace Hms.ReceptionApi.DTOs.Reception;

public class DepartmentQueueResponseDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = default!;
    public DateOnly Date { get; set; }
    public List<QueueItemDto> Queue { get; set; } = new();
}