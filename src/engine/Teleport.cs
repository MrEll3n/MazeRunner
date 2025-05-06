using OpenTK.Mathematics;

namespace ZPG;

public class Teleport
{
    public char Id { get; set; }
    public Vector3 SourcePosition { get; set; }
    public Vector3 TargetPosition { get; set; }

    public Teleport(char id, Vector3 source, Vector3 target)
    {
        Id = id;
        SourcePosition = source;
        TargetPosition = target;
    }
}