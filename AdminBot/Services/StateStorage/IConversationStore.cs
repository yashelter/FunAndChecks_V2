using AdminBot.Models;

namespace AdminBot.Services.StateStorage;

public interface IConversationStore
{
    bool TryGetValue(long key, out ActiveConversationSession session);
    void AddOrUpdate(long key, ActiveConversationSession session);
    bool TryRemove(long key, out ActiveConversationSession session);
    bool ContainsKey(long key);
}