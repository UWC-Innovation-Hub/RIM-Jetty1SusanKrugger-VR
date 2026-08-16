

public interface IGazeTarget 
{
    void OnGazeEnter();
    void OnGazeExit();
    void OnGazeDwell();
}

public interface IGazeProgressTarget
{
    void OnGazeProgress(float normalized);
}
