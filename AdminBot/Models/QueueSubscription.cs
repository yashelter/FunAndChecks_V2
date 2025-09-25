namespace AdminBot.Models;

// TODO: нужно обработать случай смерти, и совершить удаление не действительных очередей
public class QueueSubscription
{
    public long UserId { get; set; }
    public int MessageId { get; set; }
    public int EventId { get; set; }
    public required int SubjectId { get; set; }

    public string EventName { get; set; } = "";
}