using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZPG;

/// <summary>
/// Třída pro práci s OpenGL shadery – načítání, kompilace, linkování, použití a práce s uniformami.
/// </summary>
public class Shader : IDisposable
{
    /// <summary>
    /// ID OpenGL shader programu.
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// Slovník uniform proměnných a jejich lokací.
    /// </summary>
    private readonly Dictionary<string, int> uniforms = new();

    /// <summary>
    /// Vytvoří nový shaderový program z daných souborů s vertex a fragment shadery.
    /// </summary>
    /// <param name="vertexPath">Cesta k vertex shaderu.</param>
    /// <param name="fragmentPath">Cesta k fragment shaderu.</param>
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
    /// Přeloží shader ze souboru daného typu.
    /// </summary>
    /// <param name="filePath">Cesta k souboru se shaderem.</param>
    /// <param name="type">Typ shaderu (vertex, fragment atd.).</param>
    /// <returns>ID shaderu.</returns>
    /// <exception cref="Exception">Vyvolá výjimku při chybě překladu.</exception>
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
    /// Prolinkuje všechny přeložené shadery do jednoho programu.
    /// </summary>
    /// <param name="shaders">Pole ID shaderů.</param>
    /// <exception cref="Exception">Vyvolá výjimku při chybě linkování.</exception>
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
    /// Aktivuje shaderový program pro použití při vykreslování.
    /// Musí být voláno před nastavováním uniforms.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(ID);
    }

    /// <summary>
    /// Načte lokace všech aktivních uniform proměnných ve shaderu a uloží je do slovníku.
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
    /// Získá pozici (location) uniformy dle jejího jména.
    /// </summary>
    /// <param name="name">Název uniformy ve shaderu.</param>
    /// <returns>Lokace uniformy nebo -1, pokud nebyla nalezena.</returns>
    public int GetUniformLocation(string name)
    {
        if (uniforms.TryGetValue(name, out int location))
            return location;

        Console.WriteLine($"Warning: Uniform '{name}' not found.");
        return -1;
    }

    /// <summary>
    /// Nastaví hodnotu uniform proměnné podle typu (int, float, vektor, matice).
    /// </summary>
    /// <typeparam name="T">Typ dat (např. float, Vector3, Matrix4).</typeparam>
    /// <param name="name">Jméno uniformy.</param>
    /// <param name="value">Hodnota, která se má zapsat.</param>
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
    /// Uvolní prostředky spojené s tímto shaderem.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Uvnitřní metoda pro správu uvolňování zdrojů.
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
    /// Finalizér volaný garbage collectorem při zničení instance.
    /// </summary>
    ~Shader()
    {
        Dispose(false);
    }

    #endregion
}
