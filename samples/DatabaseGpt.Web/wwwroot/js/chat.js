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

        askDirect: async function (responseType) {
            this.isAsking = true;
            const text = this.message;
            this.message = '';

            this.messages.push({ role: 'user', text: text, isCompleted: true });

            this.messages.push({ role: 'assistant', text: null, isCompleted: false });
            const assistantMessage = this.messages[this.messages.length - 1];

            try {
                const response = await ask(this.conversationId, text, responseType);
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
        }        
    }));
}