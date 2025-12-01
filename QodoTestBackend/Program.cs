using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QodoTestBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// UWAGA: Zak³adam, ¿e masz ju¿ skonfigurowany ApplicationDbContext w swoim projekcie
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// ==========================================
// PONI¯EJ WKLEJAMY NASZE SCENARIUSZE TESTOWE DLA QODO MERGE
// (Wstrzykniête w Twój nowy szablon projektu)
// ==========================================

// SCENARIUSZ A: "Brzydki Kod" (Security & Bugs)
app.MapGet("/api/login-test", (string username, string password) =>
{
    // B£¥D 1: Hardcoded password
    if (username == "admin" && password == "SuperSecret123!")
    {
        return Results.Ok("Zalogowano administratora (backdoor)");
    }

    // Symulacja po³¹czenia z baz¹ (niezale¿na od EF Core powy¿ej, ¿eby pokazaæ stary kod ADO.NET)
    var connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        // connection.Open(); // Zakomentowane, ¿eby nie wywala³o b³êdu przy uruchamianiu bez prawdziwej bazy

        // B£¥D 2: Klasyczny SQL Injection
        // Qodo powinno to wy³apaæ
        string sql = "SELECT * FROM Users WHERE Username = '" + username + "' AND Password = '" + password + "'";

        using (SqlCommand command = new SqlCommand(sql, connection))
        {
            // var reader = command.ExecuteReader();
        }
    }

    return Results.Ok("Próba logowania zakoñczona");
});

// SCENARIUSZ B: "Ba³aganiarz" (Styl i Czytelnoœæ)
app.MapGet("/api/complex-calc", (int val) =>
{
    // B£¥D: Fatalne nazwy zmiennych, brak komentarzy
    int x = val;
    int y = 20;
    int z = 0;

    for (int i = 0; i < 100; i++)
    {
        if (x > 5)
        {
            z += (x * y) / 2;
            x--;
        }
        else
        {
            z += 99; // Magic number
        }
    }

    var temp = z.ToString();
    return Results.Ok(temp + " result");
});

// SCENARIUSZ C: "Pytanie do Eksperta" (Testy jednostkowe)
app.MapPost("/api/process-order", (OrderRequest req) =>
{
    var service = new OrderService();
    var result = service.CalculateTotal(req.Price, req.Quantity, req.CustomerType);
    return Results.Ok(result);
});

// ==========================================
// KONIEC SCENARIUSZY
// ==========================================

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Klasy pomocnicze (wrzucone na dó³ pliku)
public record OrderRequest(decimal Price, int Quantity, string CustomerType);

public class OrderService
{
    public decimal CalculateTotal(decimal price, int quantity, string customerType)
    {
        decimal total = price * quantity;

        if (customerType == "VIP")
        {
            return total * 0.90m;
        }
        else if (customerType == "Regular" && total > 1000)
        {
            return total * 0.95m;
        }

        return total;
    }
}