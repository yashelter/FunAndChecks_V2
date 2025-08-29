namespace AdminBot.Models;

// TODO: нужно обработать случай смерти, и совершить удаление не действительных очередей
public class QueueSubcription
{
    public long UserId { get; set; }
    public int? MessageId { get; set; }
    public int EventId { get; set; }
}