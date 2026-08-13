namespace ApiTest;

public class ApiServiceSmokeTests
{
    [Test]
    [Category("Integration")]
    public async Task TemplateTestProject_ShouldRun()
    {
        await Assert.That(GlobalHooks.App).IsNotNull();
    }
}
