using MessageBroker.RabbitMQ.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MessageBroker.RabbitMQ.RabbitMQ;

public class RabbitMQConfiguration
{
    private readonly IConfiguration _configuration;
    private const string SectionName = "RabbitMQ";
    private const string UsernameSectionName = nameof(Username);
    private const string PasswordSectionName = nameof(Password);
    private const string HostSectionName = nameof(Host);
    private const string PortSectionName = nameof(Port);
    private const string QueuePrefixeSectionName = nameof(QueuePrefixe);

    public RabbitMQConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;

        Username = GetConfigurationStringValue($"{SectionName}:{UsernameSectionName}");
        Password = GetConfigurationStringValue($"{SectionName}:{PasswordSectionName}");
        Host = GetConfigurationStringValue($"{SectionName}:{HostSectionName}");
        Port = GetConfigurationIntegerValue($"{SectionName}:{PortSectionName}");
        QueuePrefixe = GetConfigurationStringValue($"{SectionName}:{QueuePrefixeSectionName}");
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string QueuePrefixe { get; set; }

    private string GetConfigurationStringValue(string sectionKey)
    {
        var section = GetConfigurationSection(sectionKey);

        if (string.IsNullOrWhiteSpace(section.Value))
            throw new MessageBrokerConfigurationException($"Configuration value for section '{sectionKey}' should not be null or empty.");

        return section.Value;
    }

    private int GetConfigurationIntegerValue(string sectionKey)
    {
        var section = GetConfigurationSection(sectionKey);

        if (!int.TryParse(section.Value, out int intValue))
            throw new MessageBrokerConfigurationException($"Configuration value for section '{sectionKey}' should be a integer.");
        
        return intValue;
    }

    private IConfigurationSection GetConfigurationSection(string sectionKey)
    {
        var section = _configuration.GetSection(sectionKey);

        if (!section.Exists())
            throw new MessageBrokerConfigurationException($"Unable to find section '{sectionKey}' in configuration.");

        return section;
    }
}