    import express from 'express';
    const scoreRouter = express.Router();

    import {DynamoDBClient, ListBackupsCommand} from '@aws-sdk/client-dynamodb';
    const client = new DynamoDBClient( {region: "eu-west-2"} );

    import {createItem, readItem, updateItem, deleteItem, queryTable} from './dbOperations.js';
    import bodyParser from 'body-parser';
    scoreRouter.use(bodyParser.json());

    const table = 'Users';
        /*
        scoreRouter.post('/addScore', (req, res) => {
            const { player_no, player_name, scores } = req.body;
            const params = {TableName: table,
                Item: {
                    player_no: {N: player_no.toString() }, 
                    player_name: {S: player_name },
                    scores: { N: scores.toString()},
                }
            };
            console.log(params);
            createItem(params).then(data => {
                console.log(data);
            });


            //havent added error handling hyet
            res.send('Successfully added score');
        }); */

    scoreRouter.post('/addScore', async (req, res) => {
        try {
            const { player_no, player_name, scores } = req.body;
    
            if (!player_no || !player_name || !scores) {
                return res.status(400).send("Missing required fields");
            }
    
            const params = {
                TableName: table,
                Item: {
                    player_no: { N: player_no.toString() },
                    player_name: { S: player_name },
                    scores: { N: scores.toString() }
                }
            };
    
            console.log(params);
            const data = await createItem(params);  // Await the async function
    
            res.status(200).send(`Successfully added score: ${JSON.stringify(data)}`);
        } catch (error) {
            console.error(error);
            res.status(500).send("Error adding score");
        }
    });


    scoreRouter.post('/deleteScore', (req, res) => {{
        const input = req.body; //need to figure this out
        const params = {TableName: table,
            
        }
        res.send('Successfully deleted score');
    }})

    scoreRouter.get('/getScore', (req, res) => {
        //forgot why i wrote this lol
        res.send('DATA HERE');
    })
    /*
    scoreRouter.get('/highScores', (req, res) => {
        const params = {TableName: table,
            IndexName: "scoreIndex",
            KeyConditionExpression: "scores >= :zero",
            ExpressionAttributeValues: {
                ":zero": 0
            },
            ScanIndexForward: false,
            Limit: 10
        }
        queryTable(params).then((data) => {
            return data
        }
        )

        res.send('Got data');
    }) */
    scoreRouter.get('/highScores', async (req, res) => {
        try {
            const params = {
                TableName: table,
                IndexName: "scoreIndex",
                KeyConditionExpression: "player_no = :player",
                FilterExpression: "scores >= :zero",
                ExpressionAttributeValues: {
                    ":zero": { N: "0" },
                    ":player": { N: "1" }  // Replace with dynamic player_no
                },
                ScanIndexForward: false,
                Limit: 10
            };
    
            const data = await queryTable(params);
            res.status(200).json(data);
        } catch (error) {
            console.error(error);
            res.status(500).send("Error retrieving high scores");
        }
    });


    export default scoreRouter;
