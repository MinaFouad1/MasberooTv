using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MassperoTVAPI.Core.Helpers;

/// <summary>
/// Configures Swagger UI for multipart/form-data candidate endpoints so that collection fields
/// (Certifications, Educations, SkillIds, LanguageIds) render as editable string fields with
/// valid JSON examples instead of broken object arrays that serialize to "[object Object]".
/// </summary>
public class CandidateMultipartOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content == null) return;

        // Only apply to Candidate endpoints (e.g. CreateCandidateDto, UpdateCandidateDto)
        var isCandidateEndpoint = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType.Name is "CreateCandidateDto" or "UpdateCandidateDto");

        if (!isCandidateEndpoint) return;

        if (!operation.RequestBody.Content.TryGetValue("multipart/form-data", out var mediaType))
            return;

        mediaType.Schema ??= new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        mediaType.Schema.Properties ??= new Dictionary<string, OpenApiSchema>();

        var properties = mediaType.Schema.Properties;

        // Ensure the 4 collection properties are present and configured as editable strings
        // (Swashbuckle may omit them because of the custom [ModelBinder] attribute)
        EnsureProperty(properties, "LanguageIds",
            "JSON array of IDs (e.g. [1, 2]) or comma-separated numbers (1, 2)",
            "[1, 2]");

        EnsureProperty(properties, "SkillIds",
            "JSON array of IDs (e.g. [1, 2, 3]) or comma-separated numbers (1, 2, 3)",
            "[1, 2, 3]");

        EnsureProperty(properties, "Certifications",
            "JSON array of objects (e.g. [{\"name\":\"AWS Certified\",\"url\":\"https://...\"}]) or comma-separated names",
            "[{\"name\": \"AWS Certified Solutions Architect\", \"url\": \"https://aws.amazon.com\"}]");

        EnsureProperty(properties, "Educations",
            "JSON array of objects (e.g. [{\"degree\":\"Bachelor of Science\",\"university\":\"Cairo University\",\"graduationYear\":2022,\"grade\":\"Excellent\"}])",
            "[{\"degree\": \"Bachelor of Science\", \"university\": \"Cairo University\", \"graduationYear\": 2022, \"grade\": \"Excellent\"}]");
    }

    private static void EnsureProperty(IDictionary<string, OpenApiSchema> properties, string name, string description, string example)
    {
        var existingKey = properties.Keys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase)) ?? name;
        properties[existingKey] = new OpenApiSchema
        {
            Type = "string",
            Description = description,
            Example = new OpenApiString(example)
        };
    }
}
