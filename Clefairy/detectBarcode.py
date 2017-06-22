import cv2
import numpy as np
from matplotlib import pyplot as plt
import os
from six import string_types

class detectBarcode:
    def __init__(self):
        # model parameters
        self.sizeRect = (21, 7)
        self.sizeBlur = (7, 7)
        self.iterErode = 10
        self.iterDilate = 35
        self.threshGrad = 225
        self.areaLimit = 20000
        self.boxExtend = 20
        self.sortBar = 5
        
        self.claheLimit = 3.0
        self.claheGrid = (8,8)
        self.clahe = cv2.createCLAHE(clipLimit=self.claheLimit, tileGridSize=self.claheGrid)
        
        #model flags
        self.flagConsole = 0
        self.console = ''
        self.ready = 0
        
    def setInput(self, inpImg):
        if isinstance(inpImg, string_types):
            if os.path.exists(inpImg):
                self.img = cv2.imread(inpImg)
                self.ready = 1
            else:
                self.console = 'file not found'
                if self.flagConsole != 0:
                    print(self.console)
        else:
            self.img = inpImg
            self.ready = 1
     
    def setParameter(self, para, value):
        if isinstance(value, int):
            if para == 'sizeRect':
                self.sizeRect = value
            elif para == 'sizeBlur':
                self.sizeBlur = value
            elif para == 'iterErode':
                self.iterErode = value
            elif para == 'iterDilate':
                self.iterDilate = value
            elif para == 'threshGrad':
                self.threshGrad = value
            elif para == 'areaLimit':
                self.areaLimit = value
            elif para == 'boxExtend':
                self.boxExtend = value
            elif para == 'sortBar':
                self.sortBar = value
            elif para == 'claheLimit':
                self.claheLimit = value
                self.clahe = cv2.createCLAHE(clipLimit=self.claheLimit, tileGridSize=self.claheGrid)
            elif para == 'claheGrid':
                self.claheGrid = value
                self.clahe = cv2.createCLAHE(clipLimit=self.claheLimit, tileGridSize=self.claheGrid)
            else:
                self.console = 'There is not a parameter named ' + para
                if self.flagConsole != 0:
                    print(self.console)
        else:
            self.console = 'Input value type error.'
            if self.flagConsole != 0:
                print(self.console)
             
    def genOutput(self):
        if self.ready is 1:
            imgGray = cv2.cvtColor(self.img, cv2.COLOR_BGR2GRAY)
            imgGray = self.clahe.apply(imgGray)

            gradX = cv2.Sobel(imgGray, ddepth = cv2.CV_32F, dx = 0, dy = 1, ksize = -1)
            gradY = cv2.Sobel(imgGray, ddepth = cv2.CV_32F, dx = 1, dy = 0, ksize = -1)
            gradient = cv2.subtract(gradX, gradY)
            gradient = cv2.convertScaleAbs(gradient)

            blurred = cv2.blur(gradient, self.sizeBlur)
            (_, thresh) = cv2.threshold(blurred, self.threshGrad, 255, cv2.THRESH_BINARY)
            kernel = cv2.getStructuringElement(cv2.MORPH_RECT, self.sizeRect)
            closed = cv2.morphologyEx(thresh, cv2.MORPH_CLOSE, kernel)
            closed = cv2.erode(closed, None, iterations = self.iterErode)
            closed = cv2.dilate(closed, None, iterations = self.iterDilate)
            (_, self.contours, _) = cv2.findContours(closed.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

            if len(self.contours) > 1:
                self.contours = sorted(self.contours, key = cv2.contourArea, reverse = True)[:len(self.contours)]
        else:
            self.console = 'Please set an input file.'
            if self.flagConsole != 0:
                print(self.console)
                
if __name__ == '__main__':
    inpImg = '../image/receipt.jpg'
    img = cv2.imread(inpImg)
    font = cv2.FONT_HERSHEY_TRIPLEX
    colorRect = (0, 255, 0)
    
    app = detectBarcode()
    app.setInput(img)
    app.flagConsole = 1
    
    app.setParameter('iterErode', 20)
    app.setParameter('iterDilate', 40)
    app.genOutput()

    for c in app.contours:
        rect = cv2.minAreaRect(c)
        box = np.int0(cv2.boxPoints(rect))
        area = cv2.contourArea(c)
        img = cv2.drawContours(img, [box], -1, colorRect, 3)
        textX = int(box[0][0])
        textY = int(box[0][1] + 40)
        cv2.putText(img, str(area), (textX, textY), font, 1, colorRect, 2, cv2.LINE_AA)
    
    cv2.imshow('image', img)
    cv2.waitKey(0)
    cv2.destroyAllWindows()