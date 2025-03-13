import express from 'express';

const userRouter = express.Router();

userRouter.post('/register', (req, res) => {
    //get username

    //registerUser(username);
    res.send('Added user');
})

userRouter.post('/login', (req, res) => {
    //login details

    //loginUser(username);
    res.send('Logged in');
})




export default userRouter;