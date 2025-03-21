import {DynamoDBClient, GetItemCommand, PutItemCommand, UpdateItemCommand, DeleteItemCommand, QueryCommand} from '@aws-sdk/client-dynamodb';

const client = new DynamoDBClient({ region: 'eu-west-2'});

async function createItem(params) {
    const command = new PutItemCommand(params);
    try {
        const data = await client.send(command); 
        console.log("Added data successfully");
        return data; 
    }
    catch (error) {
        console.error("Error: ", error);
        throw error; 
    }
}  

async function readItem(params) {
    const command = new GetItemCommand(params);
    try {
        const data = await client.send(command);
        if (data.Item) {
                console.log("Retrieved", data.Item);
                return data;
        }
        else {
            console.log("Doesn't exist");
            return null; 
        }
    }
    catch (error) {
        console.error("Error: ", error);
        throw error;
    }
}

async function updateItem(params) {
    const command = new UpdateItemCommand(params);
    try {
        const data = await client.send(command);
        console.log("Updated: ", data);
        return data;
    }
    catch (error) {
        console.error("Error: ", error);
        throw error;
    }
}

async function deleteItem(params) {
    const command = new DeleteItemCommand(params) 
    try {
        const data = await client.send(command);
        console.log("Deleted: ", data);
        return data;
    }
    catch (error) {
        console.error("Error: ", error);
        throw error;
    }
}

async function queryTable(params) {
    const command = new QueryCommand(params);
    try {
        const data = await client.send(command);
        console.log("Data retrieved")
        return data;
    }
    catch (error) {
        console.error("Error: ", error);
        throw error;
    }
}

export {createItem, readItem, updateItem, deleteItem, queryTable};