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

void main()
{
    vec4 texColor = texture(uTexture, TexCoord);

    if (texColor.a < 0.1)
        discard;

    vec3 normal = normalize(Normal);
    vec3 lightColor = vec3(1.0);

    vec3 ambient = 0.4 * texColor.rgb;

    vec3 lightDirNorm = normalize(lightPos - FragPos);
    float diff = max(dot(normal, lightDirNorm), 0.0);
    vec3 diffuse = diff * texColor.rgb;

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDirNorm, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16.0);
    vec3 specular = spec * lightColor * 0.4;

    float theta = dot(lightDirNorm, normalize(-lightDir));
    float epsilon = cutOff - outerCutOff;
    float intensity = clamp((theta - outerCutOff) / epsilon, 0.0, 1.0);

    float dist = length(lightPos - FragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * dist + 0.032 * dist * dist);

    vec3 result = (ambient + (diffuse + specular) * intensity) * attenuation;

    FragColor = vec4(result, texColor.a);
}