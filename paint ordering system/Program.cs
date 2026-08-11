// See https://aka.ms/new-console-template for more information
using System.Reflection.Metadata;
using paint_ordering_system.Enums;
using paint_ordering_system.Models;


PaintProduct productCloud = new PaintProduct(0, "white product"
                                            , paint_ordering_system.Enums.PaintType.Glossy
                                            ,new PaintSpecification("white", 5)
                                            ,10
                                            , new Brand("aa", "this aa"));

PaintProduct productSky = new PaintProduct(1, "blue product"
                                            , paint_ordering_system.Enums.PaintType.BaseCoat
                                            ,new PaintSpecification("blue", 6)
                                            ,9
                                            , new Brand("bb", "this bb"));

PaintProduct productTree = new PaintProduct(2, "green product"
                                            , paint_ordering_system.Enums.PaintType.Matte
                                            ,new PaintSpecification("green", 7)
                                            ,8
                                            , new Brand("cc", "this cc"));


productCloud.DisplayInfo();
productSky.DisplayInfo();
productTree.DisplayInfo();




Order order = new Order(new List<OrderItem> 
{
    // new OrderItem {Product=productCloud, Quantity=1},
    new OrderItem(productCloud, 1),
    new OrderItem(productSky, 2),
    new OrderItem(productTree, 3),

},
new Payment(0, PaymentState.Failed, 100, PaymentMethod.Alipay)
);
order.DisplayOrder();