import cv2
import numpy as np
from matplotlib import pyplot as plt
import os
from six import string_types

class detectBarcodeSobel:
    def __init__(self):
        # model parameters
        self.sizeRect = (21, 7)
        self.sizeBlur = (5, 5)
        self.iterErode = 10
        self.iterDilate = 35
        self.threshGrad = 225
        self.areaLimit = 20000
        self.boxExtend = 20
        self.sortBar = 5
        
        self.claheLimit = 3.0
        self.claheGrid = (8,8)
        
        self.font = cv2.FONT_HERSHEY_TRIPLEX
        self.colorRect = (0, 255, 0)
        #model flags
        self.flagConsole = False
        self.console = ''
        self.ready = 0
        
    def setInput(self, inpImg):
        if isinstance(inpImg, string_types):
            if os.path.exists(inpImg):
                self.img = cv2.imread(inpImg)
                self.ready = 1
            else:
                self.console = 'file not found'
                if self.flagConsole is True:
                    print(self.console)
        else:
            self.img = inpImg
            self.ready = 1
     
    def setParameter(self, para, value):
        if isinstance(value, int):
            if para is 'sizeRect':
                if len(value) is 2:
                    if value[0] >= 1 and value[1] >= 1:
                        self.sizeRect = value
                    else:
                        self.console = 'The input value should bigger than zero.'
                        if self.flagConsole is True:
                            print(self.console)
                else:
                    self.console = 'The input data should be a matrix with length of 2.'
                    if self.flagConsole is 1:
                        print(self.console)
            elif para is 'sizeBlur':
                if len(value) is 2:
                    if value[0] >= 1 and value[1] >= 1:
                        self.sizeBlur = value
                    else:
                        self.console = 'The input value should bigger than zero.'
                        if self.flagConsole is True:
                            print(self.console)
                else:
                    self.console = 'The input data should be a matrix with length of 2.'
                    if self.flagConsole is True:
                        print(self.console)
            elif para is 'iterErode':
                self.iterErode = value
            elif para is 'iterDilate':
                self.iterDilate = value
            elif para is 'threshGrad':
                self.threshGrad = value
            elif para is 'areaLimit':
                self.areaLimit = value
            elif para is 'boxExtend':
                self.boxExtend = value
            elif para is 'sortBar':
                self.sortBar = value
            elif para is 'claheLimit':
                self.claheLimit = value
            elif para is 'claheGrid':
                self.claheGrid = value
            else:
                self.console = 'There is not a parameter named ' + para
                if self.flagConsole is True:
                    print(self.console)
        else:
            self.console = 'Input value type error.'
            if self.flagConsole is True:
                print(self.console)
             
    def genOutput(self):
        if self.ready >= 1:
            clahe = cv2.createCLAHE(clipLimit=self.claheLimit, tileGridSize=self.claheGrid)
            imgGray = cv2.cvtColor(self.img, cv2.COLOR_BGR2GRAY)
            imgGray = clahe.apply(imgGray)

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
            
            self.ready = 2
            return True
        else:
            self.console = 'Please set an input file.'
            if self.flagConsole is True:
                print(self.console)  
            return False

    def genPaint(self):
        if self.ready >= 2:
            for c in self.contours:
                rect = cv2.minAreaRect(c)
                box = np.int0(cv2.boxPoints(rect))
                area = cv2.contourArea(c)
                img = cv2.drawContours(self.img, [box], -1, self.colorRect, 3)
                textX = int(box[0][0])
                textY = int(box[0][1] + 40)
                cv2.putText(self.img, str(area), (textX, textY), self.font, 1, self.colorRect, 2, cv2.LINE_AA)
            return True
        else:
            self.console = 'Generate output first.'
            if self.flagConsole is True:
                print(self.console)
            return False
          
if __name__ == '__main__':
    inpImg = '../image/receipt.jpg'
    img = cv2.imread(inpImg)
    
    app = detectBarcodeSobel()
    app.setInput(img)
    app.flagConsole = True
    
    app.setParameter('iterErode', 20)
    app.setParameter('iterDilate', 20)
    statusOut = app.genOutput()
    statusPaint = app.genPaint()

    if statusOut is True and statusPaint is True:
        cv2.imshow('image', app.img)
    cv2.waitKey(0)
    cv2.destroyAllWindows()