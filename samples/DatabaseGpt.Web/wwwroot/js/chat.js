function chat() {
    Alpine.data("chat", () => ({
        conversationId: uuid(),
        message: '',
        messages: [],
        isAsking: false,

        reset: function () {
            this.conversationId = uuid();
            this.message = '';
            this.messages = [];
            this.isAsking = false;
        },

        askDirect: async function () {
            this.isAsking = true;
            const text = this.message;
            this.message = '';

            this.messages.push({ role: 'user', text: text, isCompleted: true });

            this.messages.push({ role: 'assistant', text: null, isCompleted: false });
            const assistantMessage = this.messages[this.messages.length - 1];

            try {
                const response = await ask(this.conversationId, text);
                const content = await response.json();

                const errorMessage = GetErrorMessage(response.status, content);
                if (errorMessage == null) {
                    // The request has succeeded.
                    assistantMessage.text = content.text;
                }
                else
                {
                    assistantMessage.text = errorMessage;
                }
            } catch (error) {
                assistantMessage.text = error;
            }
            finally {
                this.isAsking = false;
                assistantMessage.isCompleted = true;
            }
        },

        askStreaming: async function () {
            this.isAsking = true;
            const text = this.message;
            this.message = '';

            this.messages.push({ role: 'user', text: text, isCompleted: true });

            this.messages.push({ role: 'assistant', text: null, isCompleted: false });
            const assistantMessage = this.messages[this.messages.length - 1];

            try {
                const request = { conversationId: this.conversationId, message: text }
                const response = await fetch("/api/chat/stream", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(request)
                });

                if (response.status != 200)
                {
                    const content = await response.json();
                    const errorMessage = GetErrorMessage(response.status, content);
                    assistantMessage.text = errorMessage;
                }
                else
                {
                    // The request has succeeded. Reads the response stream.
                    const reader = response.body?.getReader();
                    if (!reader) {
                        return;
                    }

                    const decoder = new TextDecoder();

                    while (true) {
                        const { done, value } = await reader.read();
                        if (done) break;

                        const arrayString = decoder.decode(value).replace(/\[|]/g, '').replace(/^,/, '');;
                        const deltas = JSON.parse(`[${arrayString}]`);
                        const partialText = deltas.join('');
                        assistantMessage.text = (assistantMessage.text || '') + partialText;
                    }

                    reader.releaseLock();
                }
            } catch (error) {
                assistantMessage.text = error;
            }
            finally {
                this.isAsking = false;
                assistantMessage.isCompleted = true;
            }
        }
    }));
}