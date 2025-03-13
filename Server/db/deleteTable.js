import { DynamoDBClient, DeleteTableCommand } from "@aws-sdk/client-dynamodb";

const client = new DynamoDBClient({ region: "eu-west-2" });

async function deleteUsersTable() {
    const command = new DeleteTableCommand({ TableName: "Users" });
    
    try {
        const response = await client.send(command);
        console.log("User table deleted:", response);
    } catch (error) {
        console.error("Error deleting table:", error);
    }
}

deleteUsersTable();
