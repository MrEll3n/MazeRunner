using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZPG;

/// <summary>
/// Represents an OpenGL shader program – responsible for loading, compiling, linking shaders, activating them, and setting uniforms.
/// </summary>
public class Shader : IDisposable
{
    /// <summary>
    /// ID of the OpenGL shader program.
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// Dictionary of uniform variable names and their corresponding locations.
    /// </summary>
    private readonly Dictionary<string, int> uniforms = new();

    /// <summary>
    /// Creates a new shader program from the specified vertex and fragment shader source files.
    /// </summary>
    /// <param name="vertexPath">Path to the vertex shader source file.</param>
    /// <param name="fragmentPath">Path to the fragment shader source file.</param>
    public Shader(string vertexPath, string fragmentPath)
    {
        int vertexShader = CompileShader(vertexPath, ShaderType.VertexShader);
        int fragmentShader = CompileShader(fragmentPath, ShaderType.FragmentShader);
        LinkShader(vertexShader, fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        LoadUniforms();
    }

    /// <summary>
    /// Compiles a shader of the given type from a source file.
    /// </summary>
    /// <param name="filePath">Path to the shader source file.</param>
    /// <param name="type">Type of shader (e.g., vertex, fragment).</param>
    /// <returns>Compiled shader ID.</returns>
    /// <exception cref="Exception">Throws if shader compilation fails.</exception>
    private int CompileShader(string filePath, ShaderType type)
    {
        string source = File.ReadAllText(filePath);
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Error compiling {type} shader ({filePath}):\n{log}\n");
            throw new Exception($"Shader compilation failed for {filePath}");
        }

        return shader;
    }

    /// <summary>
    /// Links the compiled shaders into a single shader program.
    /// </summary>
    /// <param name="shaders">Array of compiled shader IDs.</param>
    /// <exception cref="Exception">Throws if linking fails.</exception>
    private void LinkShader(params int[] shaders)
    {
        ID = GL.CreateProgram();
        foreach (int shader in shaders)
        {
            GL.AttachShader(ID, shader);
        }
        GL.LinkProgram(ID);

        GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetProgramInfoLog(ID);
            throw new Exception($"Error linking shader program:\n{log}");
        }
    }

    /// <summary>
    /// Activates the shader program for rendering.
    /// Must be called before setting uniforms.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(ID);
    }

    /// <summary>
    /// Loads the locations of all active uniform variables and stores them in the dictionary.
    /// </summary>
    private void LoadUniforms()
    {
        GL.GetProgram(ID, GetProgramParameterName.ActiveUniforms, out int uniformCount);

        for (int i = 0; i < uniformCount; i++)
        {
            GL.GetActiveUniform(ID, i, 256, out _, out _, out _, out string name);
            int location = GL.GetUniformLocation(ID, name);

            if (location != -1)
            {
                uniforms[name] = location;
                Console.WriteLine($"Loaded uniform: {name} -> {location}");
            }
        }
    }

    /// <summary>
    /// Retrieves the location of a uniform variable by name.
    /// </summary>
    /// <param name="name">Name of the uniform variable.</param>
    /// <returns>Uniform location or -1 if not found.</returns>
    public int GetUniformLocation(string name)
    {
        if (uniforms.TryGetValue(name, out int location))
            return location;

        Console.WriteLine($"Warning: Uniform '{name}' not found.");
        return -1;
    }

    /// <summary>
    /// Sets the value of a uniform variable based on its type (int, float, vector, matrix).
    /// </summary>
    /// <typeparam name="T">Type of data (e.g., float, Vector3, Matrix4).</typeparam>
    /// <param name="name">Name of the uniform.</param>
    /// <param name="value">Value to assign.</param>
    public void SetUniform<T>(string name, T value)
    {
        int location = GetUniformLocation(name);
        if (location == -1) return;

        switch (value)
        {
            case int v:
                GL.Uniform1(location, v);
                break;
            case float v:
                GL.Uniform1(location, v);
                break;
            case Vector2 v:
                GL.Uniform2(location, v);
                break;
            case Vector3 v:
                GL.Uniform3(location, v);
                break;
            case Vector4 v:
                GL.Uniform4(location, v);
                break;
            case Matrix4 v:
                GL.UniformMatrix4(location, false, ref v);
                break;
            default:
                throw new NotSupportedException($"Uniform type {typeof(T)} is not supported.");
        }
    }

    #region IDisposable – správná správa prostředků

    private bool disposed = false;

    /// <summary>
    /// Releases the resources associated with this shader program.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Internal method for resource cleanup.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Uvolni managed prostředky zde (pokud nějaké přibyly)
            }

            GL.DeleteProgram(ID);
            Console.WriteLine("Shader program deleted.");
            disposed = true;
        }
    }

    /// <summary>
    /// Finalizer called by the garbage collector to release resources.
    /// </summary>
    ~Shader()
    {
        Dispose(false);
    }

    #endregion
}
