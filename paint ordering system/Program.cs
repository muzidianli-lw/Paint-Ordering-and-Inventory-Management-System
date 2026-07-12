// See https://aka.ms/new-console-template for more information
using System.Reflection.Metadata;
using paint_ordering_system.Models;

PaintProduct productCloud = new PaintProduct("white product"
                                            , paint_ordering_system.Enums.PaintType.Glossy
                                            ,new PaintSpecification("white", 5)
                                            ,10);

PaintProduct productSky = new PaintProduct("blue product"
                                            , paint_ordering_system.Enums.PaintType.BaseCoat
                                            ,new PaintSpecification("blue", 6)
                                            ,9);

PaintProduct productTree = new PaintProduct("green product"
                                            , paint_ordering_system.Enums.PaintType.Matte
                                            ,new PaintSpecification("green", 7)
                                            ,8);


productCloud.DisplayInfo();
productSky.DisplayInfo();
productTree.DisplayInfo();

Order order = new Order(productCloud, 5);
order.DisplayOrder();