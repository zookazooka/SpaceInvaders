import express from 'express';
import scoreRouter from './db/scoreRouter.js';
import userRouter from './db/userRouter.js';
import {WebSocketServer} from 'ws';
import {createServer} from 'http';

const app = express();
const port = 3000;
const server = createServer(app);
const wss = new WebSocketServer({ server, clientTracking: true});

let serverFull = false;

wss.on('connection', (ws) => {

    if (serverFull) {
        ws.send("Server full");
        ws.close();
        return;
    }
    console.log("Client connected");
    ws.send("Successfully connected to server");
    if (wss.clients.size == 1) {
        serverFull = true;
        wss.clients.forEach(client => {
            client.send("FULL");
        })
    }


    ws.on("message", (message) => {
        console.log("Received: ", message.toString());
        wss.clients.forEach(client => {
            if (client != ws) {
                try {
                    console.log("Sending, ", message.toString());
                    //console.log(client);
                    client.send(message);
                }
                catch (e) {
                    console.error("ERROR sending: ", e);
                }
            }
        })
    })

    ws.on("close", () => {
        console.log("Client disconnected");
        serverFull = false;
    })
})


app.get('/', (req, res) => {
    res.send('Hello');
})

app.use('/score', scoreRouter);
app.use('/user', userRouter);



server.listen(port, () => {
    console.log("Listening on port: ", port);

})

