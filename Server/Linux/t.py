import socket

host = "127.0.0.1"
port = 5000

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((host, port))
s.sendall("VK_VOLUME_MUTE".encode())

data = "foo"
data = s.recv(1024)
print("received: ", repr(data))