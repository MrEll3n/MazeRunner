#version 330 core                      // verze, muze byt vyssi

layout(location = 0) in vec3 position; // pozice bude na lokaci 0
layout(location = 1) in vec3 color;    // barva na lokaci 1

out vec3 vColor;                       // vystupem predana barva 
                                        // pozice pres gl_Position
uniform mat4 model;                    // konstanty predane z programu 
uniform mat4 view;
uniform mat4 projection;

void main() {
    gl_Position = projection * view * model * vec4(position, 1.0);
    vColor = color;
}