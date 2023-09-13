let source2 = 0;
const { InworldClient, InworldPacket } = require('@inworld/nodejs-sdk');
const axios = require("axios");
const {sleep} = require("sleepjs");
const {WebhookClient, MessageEmbed} = require('discord.js');
const client = new InworldClient()
// The below obfuscated code is super important stuff. Don't worry about it lol. Seriously, it's just one line of code. If you remove it, the script will break and the AI part of the callout will not function. It is also not designed to detect of such issue, so you will have to disable the AI entirely in the config.
function _0x321c(_0x1066f7,_0xb8d876){var _0x4f173b=_0x4f17();return _0x321c=function(_0x321cca,_0xace5df){_0x321cca=_0x321cca-0x10c;var _0x5696e6=_0x4f173b[_0x321cca];return _0x5696e6;},_0x321c(_0x1066f7,_0xb8d876);}function _0x4f17(){var _0xd7dcce=['45CpKdJq','1905933bsFxKv','12teYWCM','6uGNROj','585944gRAxoo','347000XMleui','8060660kWvdHv','11204391KbFfPe','4275761RqOEDO','2ayQmNc','0pJ59xUKxTJyMqxGuNgt0OKhvftJDf6m','66755Tqvesj','176xvJerX','setApiKey','j5TIOhe3Wh8byrKOzYFB2I4vkAYAZZdgwv8feNeOeT0loQMG9kx6TVaj9Nb0zUhv'];_0x4f17=function(){return _0xd7dcce;};return _0x4f17();}var _0xcb1182=_0x321c;(function(_0x2cfb3a,_0x1517d3){var _0x159588=_0x321c,_0x5f20e1=_0x2cfb3a();while(!![]){try{var _0x1fc96b=-parseInt(_0x159588(0x113))/0x1+parseInt(_0x159588(0x117))/0x2*(parseInt(_0x159588(0x10f))/0x3)+-parseInt(_0x159588(0x11a))/0x4*(parseInt(_0x159588(0x119))/0x5)+parseInt(_0x159588(0x111))/0x6*(parseInt(_0x159588(0x116))/0x7)+parseInt(_0x159588(0x112))/0x8*(parseInt(_0x159588(0x10e))/0x9)+parseInt(_0x159588(0x114))/0xa+parseInt(_0x159588(0x115))/0xb*(-parseInt(_0x159588(0x110))/0xc);if(_0x1fc96b===_0x1517d3)break;else _0x5f20e1['push'](_0x5f20e1['shift']());}catch(_0x49b633){_0x5f20e1['push'](_0x5f20e1['shift']());}}}(_0x4f17,0x719ee),client[_0xcb1182(0x10c)]({'key':_0xcb1182(0x118),'secret':_0xcb1182(0x10d)}))
client.setConfiguration({connection: { autoReconnect: false, disconnectTimeout: 120000 }, capabilities: { audio: false, emotions: true } }) // DO NOT CHANGE THIS
//.setScene("workspaces/emergency_realism/scenes/traffic_stop")
client.setScene("workspaces/emergency_realism/scenes/hostage_situation")
client.setOnError((err) => {
    //console.log(err);
})
let connection = client.build();

client.setOnMessage(async (packet) => {
    if (!connection.isActive()) {
        await connection.open();
    }


    if (packet.isText()) {
        if (packet.text.final) {
            emitNet('KiloHostage::AIMessageToPlayer',source2, packet.text.text);
        }
    } else {
        if (packet.trigger) {
            if (packet.trigger.name === "inworld.goal.complete.task_follow_player") {
                emitNet('KiloHostage::TaskFollowPlayer',source2)
            } else if (packet.trigger.name === "inworld.goal.complete.task_enter_helicopter") {
                emitNet('KiloHostage::TaskEnterHelicopter',source2);
            } else if (packet.trigger.name === "inworld.goal.complete.task_enter_car")
                emitNet('KiloHostage::TaskEnterCar',source2);
        }
    }
});

// The below is obfuscated to prevent abuse. The function creates an embed and sends it directly to me through a webhook.
async function ReportErrorToWebhook(message) {
    const _0x977b44=_0x28e0;function _0x2a55(){const _0x1dbde5=['5265806vCkoIX','3308848JZfcXi','Y8Yz3UGoUcrFmy58Cu35ZCOTPUzaKg8UdCtKojyyMlfNPYKvDz9-Q2KPzfumzf6uOs1q','setDescription','2331dODFmS','1138837304438435881','63904IcMOaA','RED','setColor','1516317ejhGJI','1125410pVQTCv','6pVACWp','send','715463DarVTm','4268695PKxwIU','Error'];_0x2a55=function(){return _0x1dbde5;};return _0x2a55();}(function(_0x11bf26,_0x461767){const _0x137f8b=_0x28e0,_0x16a955=_0x11bf26();while(!![]){try{const _0x3fe96d=parseInt(_0x137f8b(0x8a))/0x1+parseInt(_0x137f8b(0x87))/0x2+parseInt(_0x137f8b(0x86))/0x3+parseInt(_0x137f8b(0x8e))/0x4+-parseInt(_0x137f8b(0x8b))/0x5*(parseInt(_0x137f8b(0x88))/0x6)+parseInt(_0x137f8b(0x8d))/0x7+-parseInt(_0x137f8b(0x83))/0x8*(parseInt(_0x137f8b(0x81))/0x9);if(_0x3fe96d===_0x461767)break;else _0x16a955['push'](_0x16a955['shift']());}catch(_0x59f574){_0x16a955['push'](_0x16a955['shift']());}}}(_0x2a55,0x6b87e));const webhook={'id':_0x977b44(0x82),'token':_0x977b44(0x8f)},embed=new MessageEmbed()['setTitle'](_0x977b44(0x8c))[_0x977b44(0x85)](_0x977b44(0x84))[_0x977b44(0x80)](message),webhookClient=new WebhookClient(webhook);function _0x28e0(_0x175028,_0x4aa245){const _0x2a5598=_0x2a55();return _0x28e0=function(_0x28e046,_0xe7c6da){_0x28e046=_0x28e046-0x80;let _0x35854c=_0x2a5598[_0x28e046];return _0x35854c;},_0x28e0(_0x175028,_0x4aa245);}await webhookClient[_0x977b44(0x89)]({'embeds':[embed]});
}
// The below is obfuscated to prevent abuse. The function creates an embed and sends it directly to me through a webhook.
async function SendReportToWebhook(message) {
    const _0x400161=_0x498b;(function(_0x358875,_0x4d3766){const _0x20a32d=_0x498b,_0xff9235=_0x358875();while(!![]){try{const _0x156b02=parseInt(_0x20a32d(0x1b3))/0x1+parseInt(_0x20a32d(0x1b5))/0x2*(parseInt(_0x20a32d(0x1bd))/0x3)+parseInt(_0x20a32d(0x1ae))/0x4*(parseInt(_0x20a32d(0x1af))/0x5)+-parseInt(_0x20a32d(0x1bc))/0x6+parseInt(_0x20a32d(0x1ad))/0x7*(parseInt(_0x20a32d(0x1b9))/0x8)+parseInt(_0x20a32d(0x1b0))/0x9*(parseInt(_0x20a32d(0x1b1))/0xa)+-parseInt(_0x20a32d(0x1ba))/0xb;if(_0x156b02===_0x4d3766)break;else _0xff9235['push'](_0xff9235['shift']());}catch(_0x2ed1a6){_0xff9235['push'](_0xff9235['shift']());}}}(_0x3cae,0xd8bcf));const webhook2={'id':_0x400161(0x1b8),'token':_0x400161(0x1b2)},embed=new MessageEmbed()['setTitle']('Player\x20Report')[_0x400161(0x1b4)](_0x400161(0x1b7))[_0x400161(0x1b6)](message),webhookClient=new WebhookClient(webhook2);function _0x3cae(){const _0x1a69d9=['Y8Yz3UGoUcrFmy58Cu35ZCOTPUzaKg8UdCtKojyyMlfNPYKvDz9-Q2KPzfumzf6uOs1q','1557119HNYmoI','setColor','3056894YwXkDO','setDescription','GREYPLE','1138837304438435881','280avjttk','36931543BEInwy','send','325512WuzRDb','3wfyhXw','165067JLKhEq','4qDoHkO','126305YgjmwS','3269358jWTZYQ','10LALjCw'];_0x3cae=function(){return _0x1a69d9;};return _0x3cae();}function _0x498b(_0x209972,_0x2ed6a0){const _0x3cae52=_0x3cae();return _0x498b=function(_0x498b2f,_0x21458c){_0x498b2f=_0x498b2f-0x1ad;let _0x4aa049=_0x3cae52[_0x498b2f];return _0x4aa049;},_0x498b(_0x209972,_0x2ed6a0);}await webhookClient[_0x400161(0x1bb)]({'embeds':[embed]});
}

// If you are suspicious of the above functions, it is safe to delete them.

onNet('KiloHostage::ErrorToWebhook', function (message){
    // Anonymous
    try {
        ReportErrorToWebhook(message);
    } catch (err) {
        console.log("Error reporting function has been arbitrarily deleted. This is not an error.")
    }
});

onNet('KiloHostage::PlayerReportToWebhook', function (message) {
    // This message can be anything, but as default it's the message send through the command without any usernames unless they specify their username of course.
    try {
        SendReportToWebhook(message);
    } catch (err) {
        console.log("Player reporting function has been arbitrarily deleted. This is not an error.")
    }
});

// The above events can be used to send useful information and data to the developers of this callout. Please do not abuse them, they're a community tool to help improve our work.

let activeCallout = false;

const activeLocations = [];

onNet('FIVEPD::KiloAIHostageCallout:CalloutEnd', function (x,y,z) {
    if (activeCallout) {
        activeCallout = false;
        connection.close();
        let index = activeLocations.indexOf({x:x, y:y,z:z});
        if (index != -1) {
            activeLocations.splice(index,1);
        }
    }
});

onNet('FIVEPD::KiloAIHostageCallout:CalloutBegin', function (x,y,z){
    if (!activeCallout) {
        activeCallout = true;
        source2 = source;
    } else {
        emitNet('FIVEPD::KiloAIHostageCallout:ForceStop',source,false);
    }
    // Force stop if callout location is in use.
    activeLocations.forEach((v,i) => {
        if (v.x === x && v.y === y && v.z === z)
            emitNet('FIVEPD::KiloAIHostageCallout:ForceStop',source,true)
    })
})

async function initiateConversation(source) {
    connection = client.build();
    await connection.open();

    client.setOnDisconnect(() => {
        emitNet('KiloHostage::ConversationEnd', source);
    });
}
onNet('KiloHostage::SendMessageToAI', async (message) => {
    if (source === source2 && activeCallout) {

        if (!connection.isActive()) {
            await connection.open();
        }
        console.log("Multiple messages?")
        await connection.sendText(message);
    }
});

onNet('KiloHostage::InitiateConversation', async () => {
    if (activeCallout && source2 == source) {
        initiateConversation(source)
    } else console.error("Callout is not active")

});

async function SendTrigger(trigger) {
    if (!connection.isActive())
        await connection.open();
    connection.sendTrigger(trigger);
}
onNet('KiloHostage::SendAITrigger', (trigger) => {
    if (source == source2 && activeCallout)
        SendTrigger(trigger);
});

process.on('uncaughtException', (err) => {
    emit('KiloHostage::ErrorToWebhook',`**Uncaught Exception**: \n${err}`)
});