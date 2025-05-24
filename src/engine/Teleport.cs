using OpenTK.Mathematics;

namespace ZPG;

/// <summary>
/// Represents a teleport connection between a source and target position in the game world.
/// </summary>
public class Teleport
{
    /// <summary>
    /// Identifier of the teleport, typically a character from the map.
    /// </summary>
    public char Id { get; set; }

    /// <summary>
    /// The source position where the teleport starts.
    /// </summary>
    public Vector3 SourcePosition { get; set; }

    /// <summary>
    /// The target position where the teleport ends.
    /// </summary>
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Teleport"/> class with the given ID and positions.
    /// </summary>
    /// <param name="id">The character identifier for the teleport.</param>
    /// <param name="source">The source position of the teleport.</param>
    /// <param name="target">The target position of the teleport.</param>
    public Teleport(char id, Vector3 source, Vector3 target)
    {
        Id = id;
        SourcePosition = source;
        TargetPosition = target;
    }
}