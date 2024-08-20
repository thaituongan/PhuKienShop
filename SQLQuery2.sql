USE PK_SHOP
go
select * from Orders;
select * from Products;
select * from OrderDetails;

insert into OrderDetails (OrderId,ProductId,Quantity,UnitPrice,TotalPrice) values (2,1,2,400000,800000),
(2,2,2,3500000,7000000),
(2,3,1,1600000,1600000),
(3,1,2,400000,800000),
(3,4,1,1750000,1750000),
(3,6,1,3900000,3900000);