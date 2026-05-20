from datetime import datetime

class Logger:
    def __init__(self, name: str):
        self.name = name
    
    def log(self, level: str, message: str):
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        print(f'[{timestamp}] [{self.name}] [{level}] {message}')

    def debug(self, message: str):
        self.log('DEBUG', message)

    def info(self, message: str):
        self.log('INFO', message)

    def warning(self, message: str):
        self.log('WARNING', message)

    def error(self, message: str):
        self.log('ERROR', message)