namespace LiminalSpace
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void OnInteract(LiminalPlayerController player);
        void OnSecondaryInteract(LiminalPlayerController player) { }
    }
}
