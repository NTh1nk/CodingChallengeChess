

x = """0 0 0 0 0 0 0 0 
130 100 110 100 100 110 100 130 
50 25 40 20 20 40 25 50 
10 10 20 15 15 20 10 10 
10 10 10 10 10 10 10 10 
5 5 5 5 5 5 5 5 
-5 -5 -5 -5 -5 -5 -5 -5 
0 0 0 0 0 0 0 0"""


list = []
for index, i in enumerate(x.split(" ")):
    # print(int(i) / 8 + 3) 
    if(index % 8 > 3):
        continue
    list.append(int(int(i) / 5 + 10))

print(min(list))
print(max(list))

print("len "+ str(len(list)))

for i in list:
    if(i < 10):
        print("0" + str(i), end="")
    else:
        print(str(i), end="")
        
s = input(":::")


chunks, chunk_size = len(s), 16
l = [ s[i:i+chunk_size] for i in range(0, chunks, chunk_size) ]

print(", ".join(l))
# print([ x[i:i+chunk_size] for i in range(0, chunks, chunk_size) ])