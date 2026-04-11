namespace McFlurryMenu;

public interface ITab
{
    // The display name of the tab in the McFlurry selection bar
    string name { get; }

    // The method called every frame to render the tab's specific buttons and toggles
    void Draw();
}
