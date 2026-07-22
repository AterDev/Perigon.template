namespace ApiTest;

public class ApiServiceSmokeTests
{
    [Test]
    public async Task TemplateTestProject_ShouldRun()
    {
        await Assert.That(GlobalHooks.App).IsNotNull();
    }
}