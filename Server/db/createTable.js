import { DynamoDBClient, CreateTableCommand } from "@aws-sdk/client-dynamodb";

const client = new DynamoDBClient({ region: "eu-west-2" });

async function createUsersTable() {
    const command = new CreateTableCommand({
        TableName: "Users",
        KeySchema: [
            { AttributeName: "player_no", KeyType: "HASH" }, // Partition key
            { AttributeName: "timestamp", KeyType: "RANGE" } // Sort key
        ],
        AttributeDefinitions: [
            { AttributeName: "player_no", AttributeType: "N" },
           // { AttributeName: "player_name", AttributeType: "S" },
            { AttributeName: "scores", AttributeType: "N" },
            { AttributeName: "timestamp", AttributeType: "S" }
        ],
	BillingMode: "PAY_PER_REQUEST",
        GlobalSecondaryIndexes: [
        {
            IndexName: "scoreIndex",
            KeySchema: [
                {AttributeName: "scores", KeyType: "HASH"},
                {AttributeName: "timestamp", KeyType: "RANGE"}
            ],
            Projection: {
                ProjectionType: "ALL"
            }
    	}]
	


    });

    try {
        const response = await client.send(command);
        console.log("Created table:", response);
    } catch (error) {
        console.error("Error creating table:", error);
    }
}

createUsersTable();
