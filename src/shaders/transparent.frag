#version 330 core

in vec3 FragPos;
in vec2 TexCoord;
in vec3 Normal;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec3 lightPos;
uniform vec3 lightDir;
uniform float cutOff;
uniform float outerCutOff;
uniform vec3 viewPos;
uniform float uAlpha;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightColor = vec3(1.0);
    vec3 texColor = texture(uTexture, TexCoord).rgb;

    vec3 ambient = 0.5 * texColor;

    vec3 lightDirNorm = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDirNorm), 0.0);
    vec3 diffuse = diff * texColor * lightColor;

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDirNorm, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    vec3 specular = spec * lightColor * 0.5;

    float theta = dot(lightDirNorm, normalize(-lightDir));
    float epsilon = cutOff - outerCutOff;
    float intensity = clamp((theta - outerCutOff) / epsilon, 0.0, 1.0);

    float distance = length(lightPos - FragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * distance * distance);

    vec3 result = (ambient + (diffuse + specular) * intensity) * attenuation;
    FragColor = vec4(result, uAlpha);
}
