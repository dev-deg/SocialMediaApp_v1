document.getElementById('triggerPubSub').addEventListener('click', async function() {
    try {
        const payload = {
            filename: 'example-file-to-delete.txt'
        };

        const response = await fetch('/api/pubsub/publish', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            throw new Error('Failed to publish message');
        }

        alert('Message published successfully');
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to publish message');
    }
});