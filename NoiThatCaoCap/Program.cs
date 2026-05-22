using NoiThatCaoCap.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICategoryRepository, MockCategoryRepository>();
builder.Services.AddSingleton<IProductRepository, MockProductRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giữ giỏ hàng trong 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// === KÍCH HOẠT MIDDLEWARE SESSION TẠI ĐÂY ===
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();