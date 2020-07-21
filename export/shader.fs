#version 330 core
out vec4 FragColor;

in vec3 Normal;  
in vec3 FragPos;  
in vec2 TexCoords;
 

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    sampler2D emission;
    float shininess;
};

//Sun
struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform DirLight dirLight;

//Lamp
struct PosLight {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    //attenuation, brightness depending on distance from source
    float constant;
    float linear;
    float quadratic;
};
#define nr_point_lights 1
uniform PosLight posLights[nr_point_lights];

//Flashlight
struct SpotLight {
    vec3 position;
    vec3 direction;
 

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;

    //intensity, brightness depending on angle from source.
    float cutOff;
    float outerCutOff;
};
uniform SpotLight spotLight;

uniform Material material;
uniform vec3 viewPos; 
uniform vec3 lightColor;


vec3 directionalLight(DirLight light, vec3 normal, vec3 viewDir);  
vec3 spotlightLight(SpotLight light, vec3 normal, vec3 viewDir);
vec3 pointLight(PosLight light, vec3 normal, vec3 viewDir);

void main()
{

    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    vec3 output = vec3(0.0);
 
    //output += directionalLight(dirLight, norm, viewDir);
    
    for(int i=0; i<nr_point_lights; i++){
        output += pointLight(posLights[i], norm, viewDir);
    }
    
    output += spotlightLight(spotLight, norm, viewDir);
    
    FragColor = vec4(output, 1.0);
} 

//directional light source contribution
vec3 directionalLight(DirLight light, vec3 normal, vec3 viewDir){

    vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;

    vec3 res = ambient + diffuse + specular;

    return res;
}
//point light surce contribution
vec3 pointLight(PosLight light, vec3 normal, vec3 viewDir){
    

    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(normal, lightDir), 0.0);


    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;


    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    vec3 res = ambient + diffuse + specular;
    return res;

}
//spotlight light source contribution
vec3 spotlightLight(SpotLight light, vec3 normal, vec3 viewDir){

    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    
    vec3 reflectDir = reflect(-lightDir, normal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
            
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;  
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;  

    //intensity
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0); 

    diffuse  *= intensity;
    specular *= intensity;


    //attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    ambient  *= attenuation; 
    diffuse  *= attenuation;
    specular *= attenuation;   

    vec3 result = ambient + diffuse + specular;

    return result;
}