using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RestaurantApplication.Models.Api


{
    public class Order
    {
        public Order()
        {
            Items = new List<OrderItem>();
        }
  

        public ObjectId Id { get; set; }

        public string TableId { get; set; } = "1";

        public List<OrderItem> Items { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
    }
}
//insert data to mongoDB, input below line by line in mongosh
//use MyDemoDatabase
//db.createCollection("Order")
//db.Menu.insertOne({"Id": "1", "menuItems":"Crispy Chicken"})


//[BsonElement("Name")]
//[JsonPropertyName("Name")]
//public string menuItems { get; set; }