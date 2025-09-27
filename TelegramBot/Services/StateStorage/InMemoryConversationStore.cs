using System.Collections.Concurrent;
using TelegramBot.Models;

namespace TelegramBot.Services.StateStorage;

public class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<long, ActiveConversationSession> _activeConversations = new();

    public bool TryGetValue(long key, out ActiveConversationSession session) => _activeConversations.TryGetValue(key, out session);
    public void AddOrUpdate(long key, ActiveConversationSession session) => _activeConversations[key] = session;
    public bool TryRemove(long key, out ActiveConversationSession session) => _activeConversations.TryRemove(key, out session);
    public bool ContainsKey(long key) => _activeConversations.ContainsKey(key);
}