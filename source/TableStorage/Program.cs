using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System;
using System.Configuration;
using TableStorageHandsOn;

namespace TableStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            // 01 - Connect to your azure storage account
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //--------------------------------

            // 02 - Create a table called "customers"
            // Create table
            CloudTable table = tableClient.GetTableReference("customers");
            table.CreateIfNotExists();

            //--------------------------------

            // 03 - Insert single entity (instance of the CustomerEntity class) into the table
            // Create a new customer entity.
            CustomerEntity customer1 = new CustomerEntity("UK", "Walter");
            customer1.BirthDate = DateTime.Today;
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(customer1);

            // Execute the insert operation.
            table.Execute(insertOperation);

            //--------------------------------

            // 04 - Insert two additional CustomerEntity objects using batching (use PartitionKey "Netherlands")
            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            CustomerEntity customer2 = new CustomerEntity("Netherlands", "Jeff");
            customer2.BirthDate = DateTime.Today;

            // Create another customer entity and add it to the table.
            CustomerEntity customer3 = new CustomerEntity("Netherlands", "Ben");
            customer3.BirthDate = DateTime.Today;

            // Add both customer entities to the batch insert operation.
            batchOperation.Insert(customer2);
            batchOperation.Insert(customer3);

            // Execute the batch operation.
            table.ExecuteBatch(batchOperation);

            //--------------------------------

            // 05 - Retrieve one of the entities using TableOperation.Retrieve and print its PartitionKey using Console.WriteLine()
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>("UK", "Walter");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
                Console.WriteLine(((CustomerEntity)retrievedResult.Result).PartitionKey);
            else
                Console.WriteLine("The country could not be retrieved.");

            //--------------------------------

            // 06 - Retrieve all entities with PartitionKey "Netherlands" using TableQuery and print their RowKey using Console.WriteLine()
            // Construct the query operation for all customer entities where PartitionKey="Netherlands".
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Netherlands"));

            // Print the fields for each customer.
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Console.WriteLine("{0}", entity.PartitionKey);
            }

            //--------------------------------

            // 07 - Delete one of the entities from the table 
            // Create a retrieve operation that expects a customer entity.
            retrieveOperation = TableOperation.Retrieve<CustomerEntity>("UK", "Walter");

            // Execute the operation.
            retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity.
            CustomerEntity deleteEntity = (CustomerEntity)retrievedResult.Result;

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                Console.WriteLine("Entity deleted.");
            }

            else
                Console.WriteLine("Could not retrieve the entity.");

            //--------------------------------

            // 08 - Insert a new entity into "customers" using DynamicTableEntity instead of CustomerEntity
            // Create dynamic entity
            DynamicTableEntity dynamicCustomer = new DynamicTableEntity("USA", "White");
            dynamicCustomer.Properties.Add("Billing Number", new EntityProperty("888"));

            // Insert
            TableOperation operation = TableOperation.Insert(dynamicCustomer);
            table.Execute(operation);

            // Retrieve
            retrieveOperation = TableOperation.Retrieve("USA", "White");
            var retrievedEntity = (DynamicTableEntity)table.Execute(retrieveOperation).Result;

            Console.WriteLine("Dynamic: " + retrievedEntity.PartitionKey);

            Console.ReadKey();
        }
    }
}
