from flask import Flask, request, jsonify

app = Flask(__name__)


def analyse(voice_data):
    # 这里模拟一个分析函数，实际应用应替换为实际的分析逻辑
    return {'length': len(voice_data), 'content': 'simulated analysis'}


@app.route('/analyze', methods=['POST'])
def analyze_voice():
    if request.data:
        analysis_result = analyse(request.data)
        print("audio_received")
        return jsonify(analysis_result)
    else:
        return jsonify({'error': 'No data received'}), 400


@app.route('/camera', methods=['POST'])
def handle_camera_image():
    if request.files and 'file' in request.files:
        image = request.files['file']
        image.save("received_image.png")
        print("image received")
        return jsonify({"message": "Image received successfully"})
    else:
        return jsonify({"error": "No image received"}), 400


if __name__ == '__main__':
    app.run(debug=True, host='127.0.0.1', port=9999)
