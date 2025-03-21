import express from 'express';
const scoreRouter = express.Router();

import { DynamoDBClient, QueryCommand, PutItemCommand } from '@aws-sdk/client-dynamodb';
import bodyParser from 'body-parser';

const client = new DynamoDBClient({ region: "eu-west-2" });
scoreRouter.use(bodyParser.json());

const table = 'Users';


scoreRouter.post('/addScore', async (req, res) => {
    try {
        const { player_name, score } = req.body;

        if (!player_name || score === undefined) {
            return res.status(400).send("Missing required fields");
        }

        const timestamp = new Date().toISOString();

        const params = {
            TableName: table,
            Item: {
                player_name: { S: player_name },
                timestamp: { S: timestamp },
                scores: { N: score.toString() },
                score_partition: { S: "highscores" } 
            }
        };

        const command = new PutItemCommand(params);
        await client.send(command);

        res.status(200).send("Successfully added score");
    } catch (error) {
        console.error(error);
        res.status(500).send("Error adding score");
    }
});

scoreRouter.get('/highScores', async (req, res) => {
    try {
        const params = {
            TableName: table,
            IndexName: "scoreIndex",
            KeyConditionExpression: "score_partition = :partitionKey",
            ExpressionAttributeValues: {
                ":partitionKey": { S: "highscores" }
            },
            ScanIndexForward: false, 
            Limit: 10
        };

        const command = new QueryCommand(params);
        const data = await client.send(command);

        const formattedData = data.Items.map(item => ({
            player_name: item.player_name.S,
            score: parseInt(item.scores.N),
            timestamp: item.timestamp.S
        }));

        res.status(200).json(formattedData);
    } catch (error) {
        console.error("Error retrieving high scores:", error);
        res.status(500).send("Error retrieving high scores");
    }
});

export default scoreRouter;
