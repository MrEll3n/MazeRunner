namespace ZPG;

public interface ITriggerZone
{
    void OnPlayerEnter(Player player);
    
    // Přidáno pro lepší detekci opuštění zóny
    void OnPlayerExit(Player player);
}