using Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Add this line

builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddSingleton<ICache, RedisCacheService>();
builder.Services.AddSingleton<ILlmService, LlmService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers(); // Add this line

app.Run();

