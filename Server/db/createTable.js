import { DynamoDBClient, CreateTableCommand } from "@aws-sdk/client-dynamodb";

const client = new DynamoDBClient({ region: "eu-west-2" });

async function createUsersTable() {
    const command = new CreateTableCommand({
        TableName: "Users",
        KeySchema: [
            { AttributeName: "player_name", KeyType: "HASH" }, // Partition key
            { AttributeName: "timestamp", KeyType: "RANGE" } // Sort key
        ],
        AttributeDefinitions: [
            { AttributeName: "player_name", AttributeType: "S" },
            { AttributeName: "scores", AttributeType: "N" },
            { AttributeName: "timestamp", AttributeType: "S" },
            { AttributeName: "score_partition", AttributeType: "S" }  // New attribute for GSI partitioning
        ],
        BillingMode: "PAY_PER_REQUEST",
        GlobalSecondaryIndexes: [
            {
                IndexName: "scoreIndex",
                KeySchema: [
                    { AttributeName: "score_partition", KeyType: "HASH" }, // Static partition key
                    { AttributeName: "scores", KeyType: "RANGE" } // Sort key (DynamoDB sorts by this)
                ],
                Projection: {
                    ProjectionType: "ALL"
                }
            }
        ]
    });

    try {
        const response = await client.send(command);
        console.log("Created table:", response);
    } catch (error) {
        console.error("Error creating table:", error);
    }
}

createUsersTable();
