using LiteDB;

namespace AdminBot.Models;

public class ConversationSession
{
    [BsonId]
    public long UserId { get; set; }
    
    public long ChatId { get; set; }
    

    // Полное имя типа сценария (например, "FunAndChecks.AdminBot.Flows.CreateSubjectFlow")
    // Это нужно, чтобы после перезапуска мы могли восстановить правильный объект Flow
    public required string FlowTypeName { get; set; }

    // Номер текущего шага
    public int CurrentStepIndex { get; set; }

    // Состояние, сериализованное в JSON.
    // Мы используем JSON, чтобы можно было хранить разные типы состояний
    // (CreateSubjectState, CreateTaskState и т.д.) в одной коллекции.
    public required string StateJson { get; set; }
    
}