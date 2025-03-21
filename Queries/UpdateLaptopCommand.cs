using MediatR;

public class UpdateLaptopCommand : IRequest<Unit>
{
   public string Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public string CPU { get; set; }
    public string RAM { get; set; }
    public string GPU { get; set; }
    public string Storage { get; set; }
    public string ScreenSize { get; set; }
    public string Usage { get; set; }

    
    public UpdateLaptopCommand(string id, string name, string brand, decimal price, string cpu, string ram, string gpu, string storage, string screenSize, string usage)
    {
        Id = id;
        Name = name;
        Brand = brand;
        Price = price;
        CPU = cpu;
        RAM = ram;
        GPU = gpu;
        Storage = storage;
        ScreenSize = screenSize;
        Usage = usage;
    }
}
