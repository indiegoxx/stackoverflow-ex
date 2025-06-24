using Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Add this line

builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddSingleton<ICache, RedisCacheService>();
builder.Services.AddSingleton<ILlmBLService, LlmBLService>();
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddSingleton<ICacheLoggerService, CacheLoggerService>();

builder.Services.AddHttpClient();
// Add this after other builder.Services configurations
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add this before app.MapControllers()


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers(); // Add this line
app.UseCors();


app.Run();

