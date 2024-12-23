public class MessageDto
{
    public MessageDto()
    {
        Success = -1;
    }
    public MessageDto(int success)
    {
        Success = success;
    }

    public string? Id { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }  
    public string? Message { get; set; }
    public int Success { get; }
}